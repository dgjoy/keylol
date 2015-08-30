(function() {
	"use strict";

	keylolApp.controller("SecretTestsController", [
		"$scope", "modal",
		function($scope, modal) {
			$scope.showRegistrationForm = function() {
				modal.show({
					templateUrl: "Templates/Modal/registration.html",
					controller: "RegistrationController"
				});
			};

			$scope.showLoginPasswordForm = function() {
				modal.show({
					templateUrl: "Templates/Modal/login-password.html",
					controller: "LoginPasswordController"
				});
			};
			$scope.showEditor = function() {
				modal.show({
					templateUrl: "Templates/Modal/editor.html",
					controller: "EditorController"
				});
			};
		}
	]);
})();