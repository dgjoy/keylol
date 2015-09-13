(function() {
    "use strict";

    keylolApp.controller("RootController", [
        "$scope", "pageTitle", "union", "$http",
        function($scope, pageTitle, union, $http) {
            pageTitle.loading();
            $scope.$watch(function() {
                return union.$localStorage.login;
            }, function() {
                var login = union.$localStorage.login;
                if (login) {
                    if (login.fromRegistration) {
                        delete login.fromRegistration;
                    } else {
                        $http.get("api/user/" + login.UserId + "?includeClaims=true")
                            .then(function(response) {
                                union.$localStorage.user = response.data;
                            }, function() {
                                $http.delete("api/login/current");
                                delete union.$localStorage.login;
                                alert("登录失效，请重新登录。");
                            });
                    }
                } else {
                    delete union.$localStorage.user;
                }
            });
        }
    ]);
})();