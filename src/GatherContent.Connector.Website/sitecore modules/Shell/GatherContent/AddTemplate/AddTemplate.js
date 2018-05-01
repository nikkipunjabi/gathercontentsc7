
function ViewModel() {
    var self = this;

    this.Projects = ko.observableArray();
    this.Selected = ko.observableArray();
    this.ErrorText = ko.observable();
    this.IsError = ko.observable();

    jQuery.ajax({
        type: 'GET',
        url: '/api/sitecore/TemplatesMapping/Get',
        dataType: 'json',
        success: function (data) {
            if (data.status != "error") {
                self.Projects(data.Projects);
                self.Selected(data.Selected);
                self.IsError(false);
            } else {
                self.ErrorText("Error:" + " " + data.message);
                self.IsError(true);
            }
            jQuery(".preloader").hide();
            console.log();
            jQuery(".template_list").prepend(" ");
            jQuery(".btn_next").on("click", function (el) {
                el.stopPropagation();
                if (jQuery(".template_list").prop('scrollHeight') - 260 > jQuery(".template_list").scrollTop())
                    jQuery(".template_list").animate({ scrollTop: jQuery(".template_list").scrollTop() + 260 }, 800);
            });
            jQuery(".btn_prev").on("click", function (el) {
                el.stopPropagation();
                if (0 < jQuery(".template_list").scrollTop())
                    jQuery(".template_list").animate({ scrollTop: jQuery(".template_list").scrollTop() - 260 }, 800);
            });
        },
        async: true
    });

    closeDialog = function () {
        setTimeout(window.top.dialogClose(), 1000);
    }

    addTemplateMapping = function () {
        var dataObject = ko.toJSON(this);
        jQuery.ajax({
            url: '/api/sitecore/TemplatesMapping/Post',
            type: 'post',
            data: dataObject,
            contentType: 'application/json',
            success: function () {
                window.opener.location.reload(true);
                //setTimeout(window.top.dialogClose(), 1000);
                window.top.dialogClose();
            }
        });
    };
}




//TODO use KO
jQuery(document).ready(function () {
    jQuery(document).on("click", ".project-item", function () {
        jQuery(".project-item").children('ul').hide();
        jQuery(this).children('ul').show();
        jQuery(".project-item").removeClass("active");
        jQuery(this).addClass("active");

    });
});