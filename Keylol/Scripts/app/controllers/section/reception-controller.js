(function() {
    "use strict";

    keylolApp.controller("ReceptionController", [
        "$scope",
		function($scope) {
		    $scope.quickLinks = [
			    {
				    name: "League of Legends",
					url: "test"
			    },
				{
				    name: "Counter-Strike: Global Offense",
					url: "test"
			    },
				{
				    name: "战地：硬仗",
					url: "test"
			    }
		    ];
			$scope.recentLinks = [
			    {
				    name: "第一人称射击",
					url: "test"
			    },
				{
				    name: "即时战略",
					url: "test"
			    },
				{
				    name: "Life is Strange",
					url: "test"
			    },
				{
				    name: "桌面游戏",
					url: "test"
			    },
				{
				    name: "This war of mine",
					url: "test"
			    }
		    ];
	    }
    ]);
})();