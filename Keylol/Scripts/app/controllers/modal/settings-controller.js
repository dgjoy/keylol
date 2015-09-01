(function() {
	"use strict";

	keylolApp.controller("SettingsController", [
		"$scope", "close", "utils",
		function($scope, close, utils) {
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
		}
	]);
})();