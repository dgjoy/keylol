(function() {
	"use strict";

	keylolApp.controller("SettingsController", [
		"$scope", "close",
		function($scope, close) {
			$scope.cancel = function() {
				close();
			};
			$scope.yourVariable = "test";
		}
	]);
})();