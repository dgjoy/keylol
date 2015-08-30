(function() {
    "use strict";

    keylolApp.controller("HomeController", [
        "pageTitle", "$scope",
		function(pageTitle, $scope) {
            pageTitle.set("其乐");
        }
    ]);
})();