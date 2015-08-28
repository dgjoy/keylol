(function() {
	"use strict";

	keylolApp.controller("EditorController", [
		"$scope", "close", "$element", "utils",
		function($scope, close, $element, utils) {
			$scope.cancel = function() {
				close();
			};
			$scope.radioId = [utils.uniqueId(), utils.uniqueId(), utils.uniqueId()];
			$scope.vm = {
				content: ""
			};
		}
	]);
})();