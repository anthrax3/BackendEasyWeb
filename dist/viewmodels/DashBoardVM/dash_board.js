var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
define(["require", "exports", 'aurelia-framework', 'aurelia-fetch-client', '../..//services/appState'], function (require, exports, aurelia_framework_1, aurelia_fetch_client_1, appState_1) {
    "use strict";
    var Dashboard = (function () {
        function Dashboard(http, appState) {
            this.http = http;
            this.appState = appState;
            http.configure(function (config) {
                config
                    .useStandardConfiguration()
                    .withBaseUrl('https://api.github.com/');
            });
        }
        Dashboard.prototype.activate = function () {
            return Promise.all([
                this.http
            ]);
        };
        Dashboard.prototype.setLockr = function () {
            this.appState.setMyInFo();
        };
        Dashboard = __decorate([
            aurelia_framework_1.inject(aurelia_fetch_client_1.HttpClient, appState_1.AppState), 
            __metadata('design:paramtypes', [aurelia_fetch_client_1.HttpClient, appState_1.AppState])
        ], Dashboard);
        return Dashboard;
    }());
    exports.Dashboard = Dashboard;
});

//# sourceMappingURL=dash_board.js.map