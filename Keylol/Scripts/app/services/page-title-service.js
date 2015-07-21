(function() {
    "use strict";

    keylolApp.factory("pageTitle", [
        "$rootScope", function($rootScope) {
            return {
                set: function(title) {
                    $rootScope.pageTitle = title;
                },
                loading: function() {
                    $rootScope.pageTitle = "载入中 - 其乐";
                }
            };
        }
    ]);
})();