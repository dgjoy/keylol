(function() {
	"use strict";

	keylolApp.provider("pageTitle", function() {
		var _loadingTitle = "";
		return {
			setLoadingTitle: function(title) {
				_loadingTitle = title;
			},
			$get: [
				"$rootScope",
				function($rootScope) {
					return {
						set: function(title) {
							$rootScope.pageTitle = title;
						},
						loading: function() {
							$rootScope.pageTitle = _loadingTitle;
						}
					};
				}
			]
		};
	});
})();