(function() {
	"use strict";

	var app = angular.module("KeylolApp", [
		// Angular modules 
		"ngAnimate",
		"ngRoute",

		// Custom modules

		// 3rd Party Modules
		"angularModalService",
		"angularMoment"
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
	app.constant("amTimeAgoConfig", {
		fullDateThreshold: 1,
		fullDateFormat: "YYYY-MM-DD hh:mm",
		titleFormat: "YYYY-MM-DD hh:mm:ss"
	});

	window.keylolApp = app;
})();