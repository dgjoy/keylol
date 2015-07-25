(function() {
    "use strict";

    keylolApp.directive("klModal", function() {
        return {
            restrict: "E",
            transclude: true,
            templateUrl: "Templates/Modal/_container.html",
            scope: {
                position: "@position"
            }
        };
    });
})();