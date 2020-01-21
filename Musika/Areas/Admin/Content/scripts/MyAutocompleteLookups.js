function BindAutoCompleteLookup() {

    //Genre Lookup
    $("[data-element=Name]").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: "23.111.138.246/AdminAPI/GetSearchByGenreName/",
                data: { query: request.term },
                dataType: 'json',
                type: 'GET',
                success: function (data) {
                    if (data.length == 0) {
                        $("[data-element=GenreID]").val(0);
                    }
                    response($.map(data, function (item) {
                        return {
                            label: item.Name,
                            value: item.GenreID
                        }
                    }));
                }
            })
        },
        select: function (event, ui) {
            $("[data-element=Name]").val(ui.item.label);
            $("[data-element=GenreID]").val(ui.item.value);
            return false;
        },

        minLength: 1,
        change: function (event, ui) {

            if (ui.item == null) {
                // alert("manual");
                $("[data-element=GenreID]").val(0);
                //$("[data-element=Name]").val('');
            }
        }
    });


}