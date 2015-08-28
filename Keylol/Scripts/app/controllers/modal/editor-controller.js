(function() {
	"use strict";

	keylolApp.controller("EditorController", [
		"$scope", "close", "$element",
		function($scope, close, $element) {
			$scope.cancel = function() {
				close();
			};
		}
	]);
})();