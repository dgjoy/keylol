(function() {
    "use strict";

    keylolApp.controller("MainNavigationController", [
        "$scope", function($scope) {
            $scope.categories = [
                {
                    name: "文章分类",
                    url: "test",
                    items: [
                        {
                            name: "攻略",
                            url: "test"
                        },
                        {
                            name: "模组",
                            url: "test"
                        },
                        {
                            name: "购物",
                            url: "test"
                        },
                        {
                            name: "资料",
                            url: "test"
                        },
                        {
                            name: "研讨",
                            marginRight: "8px",
                            url: "test"
                        },
                        {
                            name: "原声",
                            url: "test"
                        },
                        {
                            name: "评测",
                            url: "test"
                        },
                        {
                            name: "感悟",
                            url: "test"
                        },
                        {
                            name: "杂谈",
                            url: "test"
                        },
                        {
                            name: "资讯",
                            url: "test"
                        },
                        {
                            name: "汉化",
                            marginRight: "8px",
                            url: "test"
                        },
                        {
                            name: "盗版",
                            url: "test"
                        },
                        {
                            name: "原画",
                            url: "test"
                        },
                        {
                            name: "状态",
                            url: "test"
                        },
                        {
                            name: "存档",
                            url: "test"
                        },
                        {
                            name: "经验",
                            url: "test"
                        },
                        {
                            name: "告示",
                            marginRight: "8px",
                            url: "test"
                        },
                        {
                            name: "影视",
                            url: "test"
                        }
                    ]
                },
                {
                    name: "游戏据点",
                    url: "test",
                    items: [
                        {
                            name: "刀塔",
                            marginRight: "6px",
                            url: "test"
                        },
                        {
                            name: "晶体管",
                            marginRight: "6px",
                            url: "test"
                        },
                        {
                            name: "使命召唤：现代战争",
                            url: "test"
                        },
                        {
                            name: "战地：硬仗",
                            marginRight: "5px",
                            url: "test"
                        },
                        {
                            name: "军团要塞2",
                            marginRight: "5px",
                            url: "test"
                        },
                        {
                            name: "求生之路2",
                            url: "test"
                        },
                        {
                            name: "刺客信条：革命",
                            marginRight: "6px",
                            url: "test"
                        },
                        {
                            name: "未转变",
                            marginRight: "6px",
                            url: "test"
                        },
                        {
                            name: "僵尸末日",
                            url: "test"
                        }
                    ]
                },
                {
                    name: "分类据点",
                    url: "test",
                    items: [
                        {
                            name: "第一人称射击",
                            marginRight: "12px",
                            url: "test"
                        },
                        {
                            name: "模拟经营",
                            marginRight: "12px",
                            url: "test"
                        },
                        {
                            name: "MOBA",
                            url: "test"
                        },
                        {
                            name: "即时战略",
                            marginRight: "12px",
                            url: "test"
                        },
                        {
                            name: "沙盒",
                            marginRight: "12px",
                            url: "test"
                        },
                        {
                            name: "塔防",
                            marginRight: "12px",
                            url: "test"
                        },
                        {
                            name: "角色扮演",
                            url: "test"
                        },
                        {
                            name: "桌面游戏",
                            marginRight: "6px",
                            url: "test"
                        },
                        {
                            name: "第三人称射击",
                            marginRight: "6px",
                            url: "test"
                        },
                        {
                            name: "横板过关",
                            url: "test"
                        }
                    ]
                },
                {
                    name: "厂商据点",
                    url: "test",
                    items: [
                        {
                            name: "威乐",
                            marginRight: "9px",
                            url: "test"
                        },
                        {
                            name: "艺电",
                            marginRight: "9px",
                            url: "test"
                        },
                        {
                            name: "育碧",
                            marginRight: "9px",
                            url: "test"
                        },
                        {
                            name: "Rockstar",
                            marginRight: "9px",
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
                            marginRight: "6px",
                            url: "test"
                        },
                        {
                            name: "卡普空",
                            marginRight: "6px",
                            url: "test"
                        },
                        {
                            name: "索尼",
                            url: "test"
                        },
                        {
                            name: "Paradox",
                            url: "test"
                        },
                        {
                            name: "独立游戏",
                            url: "test"
                        },
                        {
                            name: "微软",
                            url: "test"
                        },
                        {
                            name: "任天堂",
                            url: "test"
                        },
                        {
                            name: "SqEx",
                            url: "test"
                        },
                        {
                            name: "2K",
                            url: "test"
                        }
                    ]
                },
                {
                    name: "平台据点",
                    url: "test",
                    items: [
                        {
                            name: "Steam",
                            marginRight: "6px",
                            url: "test"
                        },
                        {
                            name: "Origin",
                            marginRight: "6px",
                            url: "test"
                        },
                        {
                            name: "PS4",
                            marginRight: "6px",
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
                            marginRight: "5px",
                            url: "test"
                        },
                        {
                            name: "战网",
                            marginRight: "6px",
                            url: "test"
                        },
                        {
                            name: "WiiU",
                            marginRight: "5px",
                            url: "test"
                        },
                        {
                            name: "PS3",
                            marginRight: "6px",
                            url: "test"
                        },
                        {
                            name: "XBOX360",
                            url: "test"
                        },
                        {
                            name: "R★SC",
                            url: "test"
                        },
                        {
                            name: "PSVita",
                            marginRight: "8px",
                            url: "test"
                        },
                        {
                            name: "iOS",
                            url: "test"
                        },
                        {
                            name: "安卓",
                            url: "test"
                        },
                        {
                            name: "GFWL",
                            url: "test"
                        }
                    ]
                }
            ];
        }
    ]);
})();