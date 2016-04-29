$(document).ready(function () {
    $("#input").on("keyup", function () {
        var list = $("#list");
        var input = $("#input").val();
        input = input.toLowerCase();
        if (input.length > 0) {
            input = input.replace(" ", "_");
            var val = {
                prefix: input
            };
            $.ajax({
                type: "POST",
                url: "http://querysuggestion.cloudapp.net/queryservice.asmx/findSuggestions",
                data: JSON.stringify(val),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data) {
                    data = data.d;
                    data = data.replace("[", "");
                    data = data.replace("]", "");
                    data = data.replace(/\"/g, "");
                    data = data.replace(/_/g, " ");
                    data = data.split(",");
                    list.empty();
                    $.each(data, function (i, item) {
                        console.log(item);
                        list.append($("<li><a href='index.html'><br /><span></span></a></li>").text(item));
                    });

                }
            });
        } else {
            list.empty();
        }
    });
});

