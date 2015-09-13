(function() {
    "use strict";

    keylolApp.factory("union", [
        "$localStorage", "$sessionStorage",
        function($localStorage, $sessionStorage) {
            return {
                $localStorage: $localStorage,
                $sessionStorage: $sessionStorage
            };
        }
    ]);
})();