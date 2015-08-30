(function() {
    "use strict";

    keylolApp.controller("PromotedPointsController", [
        "$scope",
		function($scope) {
            $scope.recommendedPoints = [
                {
                    name: "Counter-Strike Global Offensive",
                    url: "test",
                    by: {
                        name: "FPS",
                        url: "test"
                    }
                },
                {
                    name: "Dota 2",
                    url: "test",
                    by: {
                        name: "League of Legends",
                        url: "test"
                    }
                },
                {
                    name: "MOBA",
                    url: "test",
                    by: {
                        name: "League of Legends",
                        url: "test"
                    }
                },
                {
                    name: "Steam",
                    url: "test",
                    by: {
                        name: "Valve",
                        url: "test"
                    }
                },
                {
                    name: "Insurgency",
                    url: "test",
                    by: {
                        name: "Arma 2",
                        url: "test"
                    }
                }
            ];
        }
    ]);
})();