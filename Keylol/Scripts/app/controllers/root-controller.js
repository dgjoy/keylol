(function() {
    "use strict";

    keylolApp.controller("RootController", [
        "pageTitle", function(pageTitle) {
            pageTitle.loading();
        }
    ]);
})();