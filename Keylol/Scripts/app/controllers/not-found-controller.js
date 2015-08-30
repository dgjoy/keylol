(function() {
    "use strict";

    keylolApp.controller("NotFoundController", [
        "pageTitle", "$scope",
		function(pageTitle, $scope) {
            pageTitle.set("404 Not Found - 其乐");
        }
    ]);
})();