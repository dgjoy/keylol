(function() {
    "use strict";

    keylolApp.controller("TestController", [
        "pageTitle", "$scope", function(pageTitle, $scope) {
            pageTitle.set("测试页面 - 其乐");
        }
    ]);
})();