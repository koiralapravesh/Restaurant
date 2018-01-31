var food_order_array = [];  //order food
var food_category = [];     //categories
var food_order_msg = [];        //customizing food
var add_to_order = function (idstring, name, price, category, this_obj) {       //adding order
    var msg = $(this_obj).prev("table").find("input").val();;
    var today = new Date();     //for sunday menu
    if (today.getDay() === 0) {     //if the current day is sunday
        if (category === "Kids Meal" && food_category.indexOf("Burgers And Sandwiches")>=0) {   //kids meal will cost 0 with the purchase of adult food
            price = 0.00;   //setting kids meal price = 0
            food_category.splice(food_category.indexOf("Burgers And Sandwiches"),1);    //updating the total
        }
    }
    if (category === "Drinks" && food_category.indexOf("Burgers And Sandwiches") >= 0) {    //reduces $1 from drinks when making a meal
        price = price - 1.00;
        food_category.splice(food_category.indexOf("Burgers And Sandwiches"),1);
    }
    if (category === "Christmas Cookies" && food_category.indexOf("Burgers And Sandwiches") >= 0) {//makes christmas cookies $1
        price = 1.00;
        food_category.splice(food_category.indexOf("Burgers And Sandwiches"), 1);
    }
//adding order to the table along with comments and price
    food_order_array.push(idstring);    
    food_order_msg.push(name +"♠"+msg);
    food_category.push(category);
    $("#checkout_btn").fadeIn();
    //calculating the subtotal without the tax
    var subtotal = Number($("#order_subtotal").html())+Number(price);
    $("#order_subtotal").html(subtotal.toFixed(2));
    //printing customization message
    if (msg != null && msg.length > 0) {
        msg = "<br><label style='font-size:0.7em'>(" + msg + ")</label>";
    }
    $("#order_table_body").append("<tr data='"+idstring+"'><th>"+name+""+msg+"</th><th>$ "+price+"</th><th><a href='#' class='btn btn-danger' onclick='remove_from_cart(this);'>X</a></th></tr>");
};

//deleting the item from the cart
var remove_from_cart = function (e) {
    var parent_item = $(e).parent().parent().parent()[0];
    var remove_item = $(e).parent().parent()[0];
    var id_to_remove = remove_item.data;
    var item_index = food_order_array.indexOf(id_to_remove);
    food_order_array.splice(item_index, 1);
    food_order_msg.splice(item_index, 1);
    //updating the new total
    var new_price = Number($("#order_subtotal").html()) - Number($(e).parent().parent().children("th:nth-child(2)").html().substr(2));
    $("#order_subtotal").html(new_price.toFixed(2));
    if (new_price <= 0) {
        $("#checkout_btn").fadeOut();
    }
    //removing the item from the order list
    parent_item.removeChild(remove_item);
};
//checking out
var cart_checkout = function () {
    var total = Number($("#order_subtotal").html()) || 0;
    var tn = Number($("#table_num").html()) || 0;
    $.post("/Home/AddOrder", { order_ids: food_order_array,order_msg:food_order_msg, total: total, tn: tn }, function (result) {

    });
    $("#checkout_btn").html("Checkout <i class='fa fa-shopping-cart'></i>");
    $("#checkout_btn").removeAttr("onclick");
    $("#checkout_btn").off('click').on("click", function () {
        //populate table
        var subtotal = Number($("#order_subtotal").html());
        var tax = (subtotal * 8.25 / 100);
        var total = subtotal + tax;
        $("#order_checkout_st").html("$ " + subtotal.toFixed(2));
        $("#order_checkout_tax").html("$ " + tax.toFixed(2) + " (8.25 %)");
        $("#order_checkout_total").html("$ " + total.toFixed(2));
        $("#order_checkout_total").val(total);

        $(".overlay").slideDown(1000);

    });
};
//split payment
var split_payment = function (e) {
    $(e).hide();
    $($(".footer_ddl")[1]).show();  //shows splittig options to the user
};
//when the user selects the split option
var split_selected = function (e) {
    var splitters = Number($(e).val());
    var ech = Number($("#order_checkout_total").val()) / splitters;     //splitting the total
    $("#split_label").html("Splitted " + splitters + " ways. Each : $ " + ech.toFixed(2));  //printing message
    $("#split_label").fadeIn();
};
var tips_payment = function (e) {       //tips
    $(e).hide();
    $(".footer_ddl:first").show();
    tips_selected($(".footer_ddl:first")[0]);   //options for tips
}
var tips_selected = function (e) {
    var tip_percent = $(e).val();       //get the % of tips
    if (tip_percent === "C") {
        $(e).hide();
        $("#custom_tips").show();
        tip_percent = "0";
    }
    //if (tip_percent !== "C") {
        var st = Number($("#order_subtotal").html());       //add to total
        var tx = 8.25;  //tax
        var t_btps = st + st * tx / 100;
        var total = Number(t_btps);
        var tip_amt = total * Number(tip_percent) / 100;
        $("#order_checkout_tips").html("$ " + tip_amt.toFixed(2));
        $("#order_checkout_total").html((total + tip_amt).toFixed(2));
        $("#order_checkout_total").val((total + tip_amt));      //sub total of the order
    //}
    //else {

    //}
};
var custom_tips = function (e) {
    tips_selected(e);
};
$(document).ready(function () {
    //update today's special
    var today = new Date();     //check day
    if (today.getDay() === 0) {     //for sunday
        $("#today_special_label").html("Today's Special : With 1 Adult Meal, 1 Kid Meal Free!!!");
    }
    var stars_clicked = false;      //ratings
    //add hover to stars
    $(".stars>i").hover(function () {       //hover function for ratings
        var num = Number($(this).attr("data-val"));
        var arr = $(".stars>i");
        for (var i = 0; i < arr.length; i++){       
            var item = arr[i];
            console.log(item);
            if (i < num) {
                $(item).addClass("whiteStar");
            }
            else {
                $(item).removeClass("whiteStar");
            }
        }
    }, function () {
        if (!stars_clicked) {
            var arr = $(".stars>i");
            for (var i = 0; i < arr.length; i++) {
                var item = arr[i];
                $(item).removeClass("whiteStar");
            }
        }
        });
    $(".stars>i").off("click").on("click", function () {//prize 1 in 5 chance
        stars_clicked = true;
        //var num = Number($(this).attr("data-val"));
        var rand = Number((Math.random() * 4).toFixed(0));      //generate random number between 0 and 4
        $("#rn_str").html(rand);    
        switch (rand) {
            case 1:     //if random number =1, won
                $("#prize_label").html("You won a free Dessert");
                $(".stars>i").off("click"); $(".stars>i").off("mouseenter mouseleave");
                break;
            default:
                $("#prize_label").html("Better Luck Next Time !!");
                $(".stars>i").off("click"); $(".stars>i").off("mouseenter mouseleave");
        }
    })
});
var pay_cash = function () {        //paying cash
    $(".footer").hide();
    $(".heading").hide();
    $(".md_body").hide();
    $(".rating").show();
    call_help();                //calls waitstaff
};