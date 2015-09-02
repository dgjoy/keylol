(function() {
    "use strict";

    keylolApp.controller("MainNavigationController", [
        "$scope",
		function($scope) {
            $scope.categories = [
                {
                    name: "游戏",
                    url: "test",
                    items: [
                        {
                            name: "刀塔",
                            url: "test"
                        },
                        {
                            name: "晶体管",
                            url: "test"
                        },
                        {
                            name: "使命召唤：现代战争",
                            url: "test"
                        },
                        {
                            name: "战地：硬仗",
                            url: "test"
                        },
                        {
                            name: "军团要塞2",
                            url: "test"
                        },
                        {
                            name: "求生之路2",
                            url: "test"
                        }
                    ]
                },
                {
                    name: "类型",
                    url: "test",
                    items: [
                        {
                            name: "第一人称射击",
                            url: "test"
                        },
                        {
                            name: "模拟经营",
                            url: "test"
                        },
                        {
                            name: "MOBA",
                            url: "test"
                        },
                        {
                            name: "即时战略",
                            url: "test"
                        },
                        {
                            name: "沙盒",
                            url: "test"
                        },
                        {
                            name: "塔防",
                            url: "test"
                        },
                        {
                            name: "角色扮演",
                            url: "test"
                        }
                    ]
                },
                {
                    name: "厂商",
                    url: "test",
                    items: [
                        {
                            name: "威乐",
                            url: "test"
                        },
                        {
                            name: "艺电",
                            url: "test"
                        },
                        {
                            name: "育碧",
                            url: "test"
                        },
                        {
                            name: "Rockstar",
                            url: "test"
                        },
                        {
                            name: "动视",
                            url: "test"
                        },
                        {
                            name: "暴雪",
                            url: "test"
                        },
                        {
                            name: "引信",
                            url: "test"
                        },
                        {
                            name: "卡普空",
                            url: "test"
                        },
                        {
                            name: "索尼",
                            url: "test"
                        },
                        {
                            name: "Paradox",
                            url: "test"
                        }
                    ]
                },
                {
                    name: "平台",
                    url: "test",
                    items: [
                        {
                            name: "Steam",
                            url: "test"
                        },
                        {
                            name: "Origin",
                            url: "test"
                        },
                        {
                            name: "PS4",
                            url: "test"
                        },
                        {
                            name: "XBOX1",
                            url: "test"
                        },
                        {
                            name: "3DS",
                            url: "test"
                        },
                        {
                            name: "uPlay",
                            url: "test"
                        },
                        {
                            name: "战网",
                            url: "test"
                        },
                        {
                            name: "WiiU",
                            url: "test"
                        },
                        {
                            name: "PS3",
                            url: "test"
                        },
                        {
                            name: "XBOX360",
                            url: "test"
                        }
                    ]
                }
            ];
        }
    ]);
})();