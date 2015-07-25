(function() {
    "use strict";

    keylolApp.controller("RegistrationController", [
        "$scope", "ModalService", function ($scope, ModalService) {
            $scope.showRegistrationForm = function () {
                ModalService.showModal({
                    templateUrl: "Templates/Modal/registration.html",
                    controller: "RegistrationController"
                });
            };
        }
    ]);
})();