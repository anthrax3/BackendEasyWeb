﻿using Common.Logging;
using ew.application.Entities.Dto;
using ew.application.Helpers;
using ew.application.Managers;
using ew.application.Services;
using ew.common;
using ew.common.Entities;
using ew.common.Helper;
using ew.core;
using ew.core.Dto;
using ew.core.Dtos;
using ew.core.Enums;
using ew.core.Repositories;
using ew.core.Users;
using ew.gitea_wrapper;
using ew.gogs_wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ew.application.Entities
{
    public class EwhWebsite : EwhEntityBase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IWebsiteRepository _websiteRepository;
        private readonly Lazy<IEwhMapper> _ewhMapper;
        private IEwhMapper ewhMapper { get { return _ewhMapper.Value; } }
        private readonly Lazy<IEntityFactory> _entityFactory;
        private IEntityFactory entityFactory { get { return _entityFactory.Value; } }


        public EwhWebsite(IWebsiteRepository websiteRepository, IAccountRepository accountRepository, Lazy<IEwhMapper> ewhMapper, Lazy<IEntityFactory> entityFactory)
        {
            _website = new Website();
            _websiteRepository = websiteRepository;
            _accountRepository = accountRepository;
            _entityFactory = entityFactory;
            _ewhMapper = ewhMapper;
        }


        public EwhWebsite(Website website, IWebsiteRepository websiteRepository, IAccountRepository accountRepository, Lazy<IEwhMapper> ewhMapper, Lazy<IEntityFactory> entityFactory) : this(websiteRepository, accountRepository, ewhMapper, entityFactory)
        {
            _website = website;
            MapFrom(website);
        }
        public EwhWebsite(string websiteId, IWebsiteRepository websiteRepository, IAccountRepository accountRepository, Lazy<IEwhMapper> ewhMapper, Lazy<IEntityFactory> entityFactory) : this(websiteRepository, accountRepository, ewhMapper, entityFactory)
        {
            _website = _websiteRepository.Get(websiteId);
            MapFrom(_website);
        }

        #region properties
        private Website _website;
        public string WebsiteId { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Url { get; set; }
        public string WebTemplateId { get; set; }
        public string Source { get; set; }
        public string Git { get; set; }
        public string WebsiteType { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifyDate { get; set; }
        public List<DeploymentEnvironment> Stagging { get; set; }
        public List<DeploymentEnvironment> Production { get; set; }
        public List<AccountsAccessLevelOfWebsite> Accounts { get; set; }

        public string RepositoryName
        {
            get; set;
        }

        //private List<EwhAccount> _ewhAccounts { get; set; }
        //public List<EwhAccount> EwhAccounts
        //{
        //    get
        //    {
        //        if (_ewhAccounts == null) _ewhAccounts = _accountService.GetListAccount(this.Accounts.Select(x => x.AccountId).ToList());
        //        return _ewhAccounts;
        //    }
        //    set { _ewhAccounts = value; }
        //}

        public List<EwhAccount> GetListAccount()
        {
            var listAccountId = this.Accounts.Select(x => x.AccountId).ToList();
            return ewhMapper.ToEwhAccounts(_accountRepository.GetList(listAccountId));
        }

        private EwhAccount owner;
        public EwhAccount Owner
        {
            get
            {
                if (owner != null) return owner;
                var ow = this.Accounts.FirstOrDefault(x => x.AccessLevels != null && x.AccessLevels.Contains(AccessLevels.Owner.ToString()));
                Account accountAsOwner;
                if (ow == null)
                {
                    this.EwhStatus = GlobalStatus.HaveNoAnOwner;
                }
                else
                {
                    accountAsOwner = _accountRepository.Get(ow.AccountId);
                    if (accountAsOwner == null)
                    {
                        this.EwhStatus = GlobalStatus.HaveNoAnOwner;
                    }
                    else
                    {
                        owner = entityFactory.InitAccount();
                        ewhMapper.ToEntity(owner, accountAsOwner);
                    }
                }
                return owner;
            }
        }

        #endregion

        #region public methods
        public bool IsExits()
        {
            if (!string.IsNullOrEmpty(WebsiteId))
            {
                return true;
            }
            EwhStatus = core.Enums.GlobalStatus.NotFound;
            return false;
        }

        /// <summary>
        /// get account as owner of website
        /// </summary>
        /// <returns>id of account</returns>
        public string GetOwnerId()
        {
            if (IsExits())
            {
                var owner = this.Accounts.FirstOrDefault(x => x.AccessLevels != null && x.AccessLevels.Contains(AccessLevels.Owner.ToString()));
                if (owner != null)
                {
                    return owner.AccountId;
                }
            }
            return string.Empty;
        }

        public bool Save()
        {
            if (_website == null) _website = new Website();
            ewhMapper.ToEntity(_website, this);
            _website.Name = StringUtils.GetSeName(_website.Name);
            if (!IsExits() || string.IsNullOrEmpty(_website.RepositoryName))
            {
                _website.RepositoryName = string.Empty;
                var owner = this.Accounts.FirstOrDefault(x => x.AccessLevels != null && x.AccessLevels.Contains(AccessLevels.Owner.ToString()));
                if (owner != null)
                {
                    var ownerAcc = _accountRepository.Get(owner.AccountId);
                    if (ownerAcc != null) _website.RepositoryName = _website.Name;
                    var checkExitsRepo = _websiteRepository.FindAll().Any(x => x.RepositoryName.ToLower() == _website.RepositoryName.ToLower());
                    int i = 1;
                    while (checkExitsRepo)
                    {
                        _website.RepositoryName = string.Format("{0}-{1}", _website.Name, i);
                        i++;
                        checkExitsRepo = _websiteRepository.FindAll().Any(x => x.RepositoryName.ToLower() == _website.RepositoryName.ToLower());
                    }
                }
                else
                {
                    this.EwhStatus = GlobalStatus.HaveNoAnOwner;
                    return false;
                }
            }
            _websiteRepository.AddOrUpdate(_website);
            MapFrom(_website);
            return true;
        }

        public bool Create()
        {
            if (!Save()) return false;
            WebsiteId = _website.Id;
            if (_website.Accounts != null && _website.Accounts.Any())
            {
                var accList = new AccountsAccessLevelOfWebsite[_website.Accounts.Count];
                _website.Accounts.CopyTo(accList);
                foreach (var acc in accList)
                {
                    var addWebsiteAccount = new AddWebsiteAccountDto() { AccessLevels = acc.AccessLevels, AccountId = acc.AccountId };
                    this.AddAccount(addWebsiteAccount);
                }
            }
            return true;
        }

        public bool AddAccount(AddWebsiteAccountDto dto)
        {
            if (!IsExits()) return false;

            if (!ValidateHelper.Validate(dto, out ValidateResults))
            {
                EwhStatus = core.Enums.GlobalStatus.InvalidData;
                return false;
            }
            var account = _accountRepository.Get(dto.AccountId);
            if (account == null)
            {
                EwhStatus = core.Enums.GlobalStatus.Account_NotFound;
                return false;
            }

            var coreDto = new core.Dtos.AddWebsiteAccountModel() { Account = account, Website = _website, AccessLevels = dto.AccessLevels, WebsiteDisplayName = _website.DisplayName };
            _websiteRepository.AddAccount(coreDto);
            _accountRepository.AddWebsite(coreDto);

            return true;
        }

        public bool RemoveAccount(string accountId)
        {
            if (!IsExits()) return false;

            var account = _accountRepository.Get(accountId);
            if (account == null)
            {
                EwhStatus = core.Enums.GlobalStatus.Account_NotFound;
                return false;
            }
            var removeAccountDto = new RemoveWebsiteAccountModel() { Account = account, Website = _website };
            _websiteRepository.RemoveAccount(removeAccountDto);
            _accountRepository.RemoveWebsite(removeAccountDto);
            return true;
        }

        public bool UpdateAccessLevel(UpdateAccountAccessLevelToWebsite dto)
        {
            if (!IsExits()) return false;
            var websiteAccount = this.Accounts.FirstOrDefault(x => x.AccountId == dto.AccountId);
            if (websiteAccount == null)
            {
                EwhStatus = core.Enums.GlobalStatus.NotFound;
                return false;
            }
            _website.Accounts.Remove(websiteAccount);
            _website.Accounts.Add(new AccountsAccessLevelOfWebsite() { AccountId = dto.AccountId, AccessLevels = dto.AccessLevels });
            _websiteRepository.AddOrUpdate(_website);
            return true;
        }

        public bool AddStagging(UpdateDeploymentEnvironmentToWebsite dto)
        {
            if (!IsExits()) return false;
            var model = new DeploymentEnviromentModel() { Website = _website };
            if (_websiteRepository.AddOrUpdateStaging(ewhMapper.ToEntity(model, dto)))
            {
                return true;
            }
            return false;
        }

        public bool AddProduction(UpdateDeploymentEnvironmentToWebsite dto)
        {

            if (!IsExits()) return false;
            var model = new DeploymentEnviromentModel() { Website = _website };
            if (_websiteRepository.AddOrUpdateProduction(ewhMapper.ToEntity(model, dto)))
            {
                return true;
            }
            return false;
        }

        public bool RemoveStaging(string deId)
        {
            if (!IsExits()) return false;
            var model = new DeploymentEnviromentModel() { Website = _website, EnviromentId = deId };
            if (_websiteRepository.RemoveStaging(model))
            {
                return true;
            }
            return false;
        }

        public bool RemoveProduction(string deId)
        {
            if (!IsExits()) return false;
            var model = new DeploymentEnviromentModel() { Website = _website, EnviromentId = deId };
            if (_websiteRepository.RemoveProduction(model))
            {
                return true;
            }
            return false;
        }

        public bool Confirm()
        {
            if (!IsExits() || !HasOwner()) return false;
            var ewhGitea = new EwhGitea();
            if (ewhGitea.ConfirmWebsite(new gitea_wrapper.Models.ConfirmWebsiteDto(this.Owner.UserName, this.RepositoryName, this.WebTemplateId)))
            {
                this.Git = ewhGitea.WebsiteInfo.Git;
                this.Source = ewhGitea.WebsiteInfo.Source;
                this.Url = ewhGitea.WebsiteInfo.Url;
                return this.Save();
            }
            else
            {
                SyncStatus(this, ewhGitea);
            }
            return false;
        }

        public bool SetDomain(string domain)
        {
            if (!IsExits()) return false;
            domain = domain.ToLower().Trim();
            if(string.IsNullOrEmpty(domain) || domain.Contains("://") || domain.StartsWith("*"))
            {
                this.EwhStatus = GlobalStatus.Invalid;
                return false;
            }
            if(_websiteRepository.FindAll().Any(x=>x.Url==domain && x.Id != this.WebsiteId))
            {
                this.EwhStatus = GlobalStatus.AlreadyExists;
                return false;
            }
            this.Url = domain;
            return Save();
        }
        public bool HasOwner()
        {
            if (Owner == null)
            {
                this.EwhStatus = GlobalStatus.HaveNoAnOwner;
                return false;
            }
            return true;
        }

        public bool InitGogSource()
        {
            var owner = this.Accounts.FirstOrDefault(x => x.AccessLevels != null && x.AccessLevels.Contains(AccessLevels.Owner.ToString()));
            Account accountAsOwner;
            if (owner == null)
            {
                this.EwhStatus = GlobalStatus.HaveNoAnOwner;
                return false;
            }
            else
            {
                accountAsOwner = _accountRepository.Get(owner.AccountId);
                if (accountAsOwner == null)
                {
                    this.EwhStatus = GlobalStatus.HaveNoAnOwner;
                    return false;
                }
            }

            var ewhSource = new EwhSource();
            // create source
            if (ewhSource.CreateRepository(accountAsOwner.UserName, this.RepositoryName))
            {
                this.Source = ewhSource.RepositoryAdded.Url;
                this.Save();
                return true;
            }
            else
            {
                this.EwhStatus = GlobalStatus.UnSuccess;
            }
            return false;
        }

        public async Task<bool> SelfSync()
        {
            var accountIdsManageWebsite = _accountRepository.FindAll().Where(x => x.Websites != null && x.Websites.Any(y => y.WebsiteId == this.WebsiteId)).Select(x => x.Id).ToList();

            if (IsExits())
            {
                EwhLogger.Common.Info("SeftSync start");

                //var newStaggings = this.Stagging.Where(x=>x.Id==)
                var newAccountsManageWebsite = this.Accounts.Where(x => !accountIdsManageWebsite.Contains(x.AccountId)).ToList();
                var removeAccountsManageWebsite = accountIdsManageWebsite.Where(x => !(this.Accounts.Select(y => y.AccountId).ToList()).Contains(x)).ToList();
                foreach (var item in newAccountsManageWebsite)
                {
                    this.AddAccount(new AddWebsiteAccountDto() { AccountId = item.AccountId, AccessLevels = item.AccessLevels });
                }
                foreach (var id in removeAccountsManageWebsite)
                {
                    this.RemoveAccount(id);
                }
                EwhLogger.Common.Info("SeftSync end");
            }
            return true;
        }

        #endregion

        #region methods
        private void MapFrom(Website website)
        {
            if (website == null) return;

            WebsiteId = website.Id;
            Name = website.Name;
            DisplayName = website.DisplayName;
            Url = website.Url;
            Stagging = website.Stagging ?? new List<DeploymentEnvironment>();
            Production = website.Production ?? new List<DeploymentEnvironment>();
            Accounts = website.Accounts ?? new List<AccountsAccessLevelOfWebsite>();
            this.WebTemplateId = website.WebTemplateId;
            this.Source = website.Source;
            this.Git = website.Git;
            this.WebsiteType = website.WebsiteType;
            this.CreatedDate = website.CreatedDate;
            this.LastModifyDate = website.LastModifyDate;
            this.RepositoryName = website.RepositoryName;
        }


        #endregion


    }
}
