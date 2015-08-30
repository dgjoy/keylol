(function() {
	"use strict";

	keylolApp.controller("SecretTestsController", [
		"$scope", "ModalService",
		function($scope, ModalService) {
			$scope.showRegistrationForm = function() {
				ModalService.showModal({
					templateUrl: "Templates/Modal/registration.html",
					controller: "RegistrationController"
				});
			};

			$scope.showLoginPasswordForm = function() {
				ModalService.showModal({
					templateUrl: "Templates/Modal/login-password.html",
					controller: "LoginPasswordController"
				});
			};
			$scope.showEditor = function() {
				ModalService.showModal({
					templateUrl: "Templates/Modal/editor.html",
					controller: "EditorController"
				});
			};
		}
	]);
})();