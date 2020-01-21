<html>
<head>
    <title>Musika</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <link href="Content/Ticketing.css" rel="stylesheet" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css">
</head>

<body>
    <form class="form_cover_main">
        <div class="biling-cover">
            <h2>Billing</h2>
            <span>
                <label>Name</label>
                <input type="text" name="name" required />
            </span>
            <span>
                <label>Email</label>
                <input type="Email" name="name" required />
            </span>
            <span>
                <label>Address</label>
                <input type="text" name="name" required />
            </span>
            <span>
                <label>Country/State</label>
                <input type="text" name="name" required />
            </span>
            <span>
                <label>City</label>
                <input type="text" name="name" required />
            </span>
            <span>
                <label>Postal Code</label>
                <input type="text" name="name" required />
            </span>
            <span>
                <label>Phone Number</label>
                <input type="text" name="name" required />
            </span>
        </div>

        <div class="biling-cover">
            <h2>Card Info</h2>
            <div class="card-cover">
                <img src="Content/img/cards.png"></div>
            <span>
                <label>Card Number</label>
                <input type="text" name="name" required />
            </span>
            <span>
                <label>Name on Card</label>
                <input type="text" name="name" required />
            </span>

            <label class="epiry-dates">
                <span>
                    <label>Expiry Month</label>
                    <select>
                        <option>Jan</option>
                        <option>Feb</option>
                        <option>March</option>
                        <option>April</option>
                        <option>May</option>
                        <option>June</option>
                        <option>July</option>
                        <option>Aug</option>
                        <option>Sep</option>
                        <option>Oct</option>
                        <option>Nov</option>
                        <option>Dec</option>
                    </select>
                </span>
            </label>

            <label class="epiry-dates">
                <span>
                    <label>Expiry Date</label>
                    <select>
                        <option>2018</option>
                        <option>2019</option>
                        <option>2020</option>
                        <option>2021</option>
                        <option>2022</option>
                        <option>2023</option>
                        <option>2024</option>
                        <option>2025</option>
                        <option>2026</option>
                        <option>2027</option>
                        <option>2028</option>
                        <option>2029</option>
                    </select>
                </span>
            </label>

            <span class="cvv-code">
                <label>CVV</label>
                <input type="text" name="name" required />
                <i class="fa fa-credit-card-alt" aria-hidden="true"></i>
            </span>
        </div>
        <div class="make_payment_btn">
            <button>Make Payment</button>
        </div>

    </form>
</body>
</html>
