(function() {
    "use strict";

    keylolApp.controller("TestController", [
        "$rootScope", "$scope", function($rootScope, $scope) {
            $rootScope.title = "测试";
            $scope.testText = "hahahaha";
        }
    ]);
})();