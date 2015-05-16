$(function () {
    "use strict";

    var inputBox = $("#text-input-area");
    inputBox.autogrow();

    $(".apply-font-button").click(function() {
        var l = Ladda.create(this);
        l.start();
        $.post("Home/GetFontSubset", { fontName:$(this).data("font-name"), text: inputBox.val() }).done(function(data) {
            inputBox.fontface({
                fontName: data.fontName,
                fileName: data.fileName,
                fontStack: $("body").css("font-family")
            });
        }).always(function() {
            setTimeout(l.stop, 100);
        });
    });
});