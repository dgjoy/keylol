$("#input_apiKey")
    .attr("placeholder", "Your Access Token Here")
    .unbind("change")
    .change(function () {
    var key = $("#input_apiKey")[0].value;
    if (key && key.trim() !== "") {
        window.swaggerUi.api.clientAuthorizations.add("key", new window.SwaggerClient.ApiKeyAuthorization("Authorization", "Bearer " + key, "header"));
    }
});