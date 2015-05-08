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
            $routeProvider.when("/test", {
                templateUrl: "Templates/test.html"
            });

            $locationProvider.html5Mode(true);
        }
    ]);

    window.keylolApp = app;
})();