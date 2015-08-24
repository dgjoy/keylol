(function() {
	"use strict";

	keylolApp.controller("RegistrationController", [
		"$scope", "close", "ModalService", function($scope, close, ModalService) {
			$scope.cancel = function() {
				close();
			};
			$scope.showRegistrationForm = function() {
                ModalService.showModal({
                    templateUrl: "Templates/Modal/registration.html",
                    controller: "RegistrationController"
                });
            };
		}
	]);
})();