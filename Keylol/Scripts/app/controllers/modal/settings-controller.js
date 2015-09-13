(function() {
    "use strict";

    keylolApp.controller("SettingsController", [
        "$scope", "close", "utils", "$http", "union",
        function($scope, close, utils, $http, union) {
            $scope.cancel = function() {
                close();
            };
            $scope.page = "profiles";
            $scope.uniqueIds = {};
            for (var i = 0; i < 22; ++i) {
                $scope.uniqueIds[i] = utils.uniqueId();
            }
            $scope.geetestId = utils.createGeetest("float", function(result, geetest) {

            });
            $scope.logout = function () {
                if (confirm("确认退出登录？")) {
                    $http.delete("api/login/current").then(function() {
                        delete union.$localStorage.login;
                        alert("成功退出登录。");
                        close();
                    }, function(response) {
                        alert(response.data);
                    });
                }
            };
        }
    ]);
})();