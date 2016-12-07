﻿using ew.application;
using ew.application.Entities;
using ew.application.Entities.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ew.webapi.Controllers
{
    /// <summary>
    /// Website
    /// </summary>
    [RoutePrefix("Websites")]
    public class WebsiteController : BaseApiController
    {
        private readonly IWebsiteManager _websiteManager;
        private readonly IAccountManager _accountManager;

        public WebsiteController(IWebsiteManager websiteManager, IAccountManager accountManager)
        {
            _websiteManager = websiteManager;
            _accountManager = accountManager;
        }

        /// <summary>
        /// Lấy danh sách websites
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public IHttpActionResult Websites()
        {
            var data = _websiteManager.GetListEwhWebsite();

            return Ok(data);
        }

        /// <summary>
        /// Tạo mới website
        /// </summary>
        /// <param name="dto">AddNewWebsite model</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public IHttpActionResult CreateWebsite(CreateWebsiteDto dto)
        {
            if (ModelState.IsValid)
            {
                if (_websiteManager.CreateWebsite(dto))
                {
                    return Ok(_websiteManager.EwhWebsiteAdded);
                }
                return NoOK(_websiteManager as EwhEntityBase);
            }
            return InvalidRequest();
        }

        /// <summary>
        /// Lấy thông tin chi tiết 1 website
        /// </summary>
        /// <param name="websiteId">Id của website</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{websiteId}")]
        public IHttpActionResult Websites(string websiteId)
        {
            var data = _websiteManager.GetEwhWebsite(websiteId);
            if (data == null) return NotFound();
            return Ok(data);
        }

        /// <summary>
        /// Lấy danh sách user được quản trị website
        /// </summary>
        /// <param name="websiteId">id của website</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{websiteId}/users")]
        public IHttpActionResult Users(string websiteId)
        {
            var ewhWebsite = _websiteManager.GetEwhWebsite(websiteId);
            if (ewhWebsite == null) return NotFound();
            return Ok(ewhWebsite.GetListAccount());
        }

        /// <summary>
        /// Danh sách môi trường stagging của website
        /// </summary>
        /// <param name="websiteId">id của website</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{websiteId}/staggings")]
        public IHttpActionResult Staggings(string websiteId)
        {
            var ewhWebsite = _websiteManager.GetEwhWebsite(websiteId);
            if (ewhWebsite == null) return NotFound();
            return Ok(ewhWebsite.Stagging);
        }

        /// <summary>
        /// Thêm mới 1 môi trường stagging cho website
        /// </summary>
        /// <param name="websiteId">id của website</param>
        /// <param name="dto">thông tin môi trường</param>
        /// <returns></returns>
        [HttpPut, HttpPatch]
        [Route("{websiteId}/staggings")]
        public IHttpActionResult AddStagging(string websiteId, UpdateDeploymentEnvironmentToWebsite dto)
        {
            if (!ModelState.IsValid) return InvalidRequest();

            var ewhWebsite = _websiteManager.GetEwhWebsite(websiteId);
            if (ewhWebsite == null) return NotFound();
            if (ewhWebsite.AddStagging(dto))
            {
                return Ok();
            }
            else
            {
                return NoOK(ewhWebsite);
            }
        }


        /// <summary>
        /// Xóa 1 môi trường stagging khỏi website
        /// </summary>
        /// <param name="websiteId">id website</param>
        /// <param name="staggingId">id môi trường stagging</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{websiteId}/staggings/{staggingId}")]
        public IHttpActionResult RemoveStagging(string websiteId, string staggingId)
        {
            if (!ModelState.IsValid) return InvalidRequest();

            var ewhWebsite = _websiteManager.GetEwhWebsite(websiteId);
            if (ewhWebsite == null) return NotFound();
            if (ewhWebsite.RemoveStaging(staggingId))
            {
                return Ok();
            }
            else
            {
                return NoOK(ewhWebsite);
            }
        }


        /// <summary>
        /// Danh sách môi trường production của website
        /// </summary>
        /// <param name="websiteId">id của website</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{websiteId}/productions")]
        public IHttpActionResult Productions(string websiteId)
        {
            var ewhWebsite = _websiteManager.GetEwhWebsite(websiteId);
            if (ewhWebsite == null) return NotFound();
            return Ok(ewhWebsite.Production);
        }

        /// <summary>
        /// Thêm mới 1 môi trường production cho website
        /// </summary>
        /// <param name="websiteId">id của website</param>
        /// <param name="dto">thông tin môi trường</param>
        /// <returns></returns>
        [HttpPut, HttpPatch]
        [Route("{websiteId}/productions")]
        public IHttpActionResult AddProduction(string websiteId, UpdateDeploymentEnvironmentToWebsite dto)
        {
            if (!ModelState.IsValid) return InvalidRequest();

            var ewhWebsite = _websiteManager.GetEwhWebsite(websiteId);
            if (ewhWebsite == null) return NotFound();
            if (ewhWebsite.AddProduction(dto))
            {
                return Ok();
            }
            else
            {
                return NoOK(ewhWebsite);
            }
        }


        /// <summary>
        /// Xóa 1 môi trường production khỏi website
        /// </summary>
        /// <param name="websiteId">id website</param>
        /// <param name="productionId">id môi trường production</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{websiteId}/productions/{productionId}")]
        public IHttpActionResult RemoveProduction(string websiteId, string productionId)
        {
            if (!ModelState.IsValid) return InvalidRequest();

            var ewhWebsite = _websiteManager.GetEwhWebsite(websiteId);
            if (ewhWebsite == null) return NotFound();
            if (ewhWebsite.RemoveProduction(productionId))
            {
                return Ok();
            }
            else
            {
                return NoOK(ewhWebsite);
            }
        }


    }
}
