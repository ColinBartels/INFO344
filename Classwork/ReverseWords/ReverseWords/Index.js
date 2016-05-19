$(document).ready(function () {
    $("#input").on("keyup", function () {
        var list = $("#list");
        var input = $("#input").val();
        if (input.length > 0) {
            var val = {
                input: input
            };
            console.log("here");
            $.ajax({
                type: "POST",
                url: "http://localhost:60636/ReverseWords.asmx/ReverseWord",
                data: JSON.stringify(val),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data) {
                    console.log(data);
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