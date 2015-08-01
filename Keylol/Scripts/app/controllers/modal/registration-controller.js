(function() {
	"use strict";

	keylolApp.controller("RegistrationController", [
		"$scope", "close", "ModalService", function($scope, close, ModalService) {
			$scope.cancel = function() {
				close();
			};
		}
	]);
})();