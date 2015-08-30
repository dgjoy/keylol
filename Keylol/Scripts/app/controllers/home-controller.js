(function() {
    "use strict";

    keylolApp.controller("HomeController", [
        "pageTitle", "$scope", "ModalService",
		function(pageTitle, $scope, ModalService) {
            pageTitle.set("其乐");
        }
    ]);
})();