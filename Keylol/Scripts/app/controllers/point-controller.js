(function() {
    "use strict";

    keylolApp.controller("PointController", [
        "pageTitle", "$scope",
		function(pageTitle, $scope) {
            pageTitle.set("据点 - 其乐");
		}
    ]);
})();