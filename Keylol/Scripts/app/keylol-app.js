(function() {
    "use strict";

    var app = angular.module("KeylolApp", [
        // Angular modules 
        "ngAnimate",
        "ngRoute"

        // Custom modules 

        // 3rd Party Modules
    ]);
    app.config([
        "$routeProvider", "$locationProvider", function($routeProvider, $locationProvider) {
            $locationProvider.html5Mode(true);

            $routeProvider.when("/", {
                templateUrl: "Templates/home.html",
                controller: "HomeController"
            }).when("/test", {
                templateUrl: "Templates/test.html",
                controller: "TestController"
            });
        }
    ]);

    window.keylolApp = app;
})();