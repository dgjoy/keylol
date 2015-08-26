(function() {
    "use strict";

    keylolApp.controller("HomeController", [
        "pageTitle", "$scope", "ModalService", function(pageTitle, $scope, ModalService) {
            pageTitle.set("其乐");

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