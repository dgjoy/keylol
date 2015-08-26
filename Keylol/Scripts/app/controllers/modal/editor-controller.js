(function() {
	"use strict";

	keylolApp.controller("EditorController", [
		"$scope", "close",
		function($scope, close) {
			$scope.cancel = function() {
				close();
			}
		}
	]);
})();