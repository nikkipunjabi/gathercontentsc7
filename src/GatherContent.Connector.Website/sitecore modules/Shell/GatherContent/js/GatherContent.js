function getUrlVars() {
    var vars = [], hash;
    var hashes = location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}

dateSort = function (a, b) {
    var a1 = moment(a, "'DD/MM/YYYY hh:mm A").format("YYYY-MM-DD HH:mm");
    var b1 = moment(b, "'DD/MM/YYYY hh:mm A").format("YYYY-MM-DD HH:mm");
    if (a1 == b1) {
        return 0;
    }
    if (a1 < b1) {
        return -1;
    }
    return 1;
}

function initTooltip() {
    jQuery("tr td").each(function (i) {
        if (jQuery(this).outerWidth() < this.scrollWidth) {
            simple_tooltip(this, "tooltip", i);
        }
    });
}
function treeInit() {
    jQuery(document).on("click", function (el) {
        if (jQuery(el.target).closest(".tree_wrap").length) return;
        if (jQuery(el.target).closest(".dynatree-expander").length) return;
        if (jQuery(el.target).closest("#location-droptree").length) return;
        if (jQuery(el.target).closest(".input-block").length) return;
        jQuery(".tree_init").hide();
        jQuery("#location-droptree").hide();

        return true;
    })
}
function resizeTableHead() {
    jQuery("thead th.cell_resize").each(function () {
        jQuery(this).find("div").css("width", jQuery(this).width());
    });
    jQuery("thead th div").each(function () {
        if (jQuery(this).height() > 18) {
            jQuery(this).css("padding-top", 0);
            jQuery(this).css("margin-top", 7);
        }
    });
}

function simple_tooltip(target_items, name, i) {
    jQuery("body").append("<div class='" + name + "' id='" + name + i + "'><p>" + jQuery(target_items).text() + "</p></div>");
    var my_tooltip = jQuery("#" + name + i);

    jQuery(target_items).mouseover(function () {
        my_tooltip.css({ opacity: 0.8, display: "none" }).fadeIn(0);
    }).mousemove(function (kmouse) {
        my_tooltip.css({ left: kmouse.pageX - 120, top: kmouse.pageY + 20 });
    }).mouseout(function () {
        my_tooltip.fadeOut(0);
    });
}
function document_resize() {
    jQuery(window).resize(function () {
        jQuery(".tooltip").remove();
        jQuery("tr td").each(function (i) {
            if (jQuery(this).outerWidth() < this.scrollWidth) {
                simple_tooltip(this, "tooltip", i);
            }
        });
        jQuery("thead th div").each(function () {
            if (jQuery(this).height() > 18) {
                jQuery(this).css("padding-top", 0);
                jQuery(this).css("margin-top", 7);
            }
        });
    });
}

jQuery(function () {
    treeInit();

    jQuery(document).on("click", ".dynatree-title", function (el) { el.preventDefault(); })
    jQuery(window).resize(function () {
        jQuery(".table_import_scroll").css("max-height", jQuery("body").height() - 250);
    });
    jQuery(".table_import_scroll").css("max-height", jQuery("body").height() - 250);
});


