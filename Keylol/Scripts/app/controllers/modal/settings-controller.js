(function() {
	"use strict";

	keylolApp.controller("SettingsController", [
		"$scope", "close",
		function ($scope, close) {
			$scope.cancel = function() {
				close();
			};
			$scope.changepage = function (p) {
			    $scope.settings_page = { 1: false, 2: false, 3: false, 4: false };;
			    $scope.settings_page[p] = true;
			    console.log($scope.settings_page);
			};
			$scope.yourVariable = "test";
			$scope.settings_page = { 1: false, 2: false, 3: false, 4: false };;
			$scope.settings_page[1] = true;
		}
	]);
})();