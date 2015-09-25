$(document).ready(function () {

    $('#toolbar .hamburger').on('click', function () {
        $(this).parent().toggleClass('open');
        if ($('#hButton').hasClass('glyphicon-menu-right')) {
            $('#hButton').removeClass('glyphicon-menu-right');
            $('#hButton').addClass('glyphicon-menu-left');
        }
        else {
            $('#hButton').removeClass('glyphicon-menu-left');
            $('#hButton').addClass('glyphicon-menu-right');
        }
    });

    createMap();
}
);
var currentLegend = null;
function createMap() {
    var mapLayer = L.tileLayer('http://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors, &copy; <a href="http://cartodb.com/attributions">CartoDB</a>',
        minZoom: 7,
        maxZoom: 14
    });


    var secimLayer = L.tileLayer('tiles/istSecim/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors, &copy; <a href="http://cartodb.com/attributions">CartoDB</a>',
        tms: true,
        minZoom: 7,
        maxZoom: 14,
        bounds: [
                new L.LatLng(41.525249, 29.929661),
                new L.LatLng(40.802731, 27.998461)
        ]
    });

    var nufusLayer = L.tileLayer('tiles/istNufus/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors, &copy; <a href="http://cartodb.com/attributions">CartoDB</a>',
        tms: true,
        minZoom: 7,
        maxZoom: 14,
        bounds: [
                new L.LatLng(41.525249, 29.929661),
                new L.LatLng(40.802731, 27.998461)
        ]
    });

    var emptyLayer = L.tileLayer('', {
        minZoom: 7,
        maxZoom: 14
    });

    var overlayMaps = {
        "Seçim Sonuçları [2015]": secimLayer,
        "Nüfus [2014]": nufusLayer,
        "Boş": emptyLayer
    };

    var map = L.map('map', {
        center: [41.2050619, 29.0253431],
        zoomControl: false,
        zoom: 10,
        layers: [mapLayer, secimLayer]
    });

    L.control.layers(overlayMaps, null, { collapsed: false }).addTo(map);

    new L.Control.Zoom({
        position: 'topright'
    }).addTo(map);

    // LEGENDs
    var secimLegend = L.control({ position: 'bottomright' });
    secimLegend.onAdd = function (map) {
        var div = L.DomUtil.create('div', 'info legend'),
            items = ["AKP", "CHP", "MHP", "HDP", "Diğer"],
            labels = [];

        // loop through our density intervals and generate a label with a colored square for each interval
        div.innerHTML = '<h4><strong>1 nokta = 1 seçmen</strong></h4>';
        for (var i = 0; i < items.length; i++) {
            div.innerHTML +=
                '<i style="background:' + getColor(items[i]) + '"></i> ' +
                items[i] + '<br/>';
        }

        return div;
    };

    var nufusLegend = L.control({ position: 'bottomright' });
    nufusLegend.onAdd = function (map) {
        var div = L.DomUtil.create('div', 'info legend'),
            items = ["18- Nüfus", "18+ Nüfus"],
            labels = [];

        // loop through our density intervals and generate a label with a colored square for each interval
        div.innerHTML = '<h4><strong>1 nokta = 1 insan</strong></h4>';
        for (var i = 0; i < items.length; i++) {
            div.innerHTML +=
                '<i style="background:' + getColor(items[i]) + '"></i> ' +
                items[i] + '<br/>';
        }

        return div;
    };

    secimLegend.addTo(map);
    currentLegend = secimLegend;

    map.on('baselayerchange', function (eventLayer) {
        // Switch to the Population legend...
        if (eventLayer.name === 'Seçim Sonuçları [2015]') {
            if (currentLegend != null) this.removeControl(currentLegend);
            secimLegend.addTo(this);
            currentLegend = secimLegend;
        } else if (eventLayer.name === 'Nüfus [2014]') { // Or switch to the Population Change legend...
            if (currentLegend != null) this.removeControl(currentLegend);
            nufusLegend.addTo(this);
            currentLegend = nufusLegend;
        }
        else {
            this.removeControl(currentLegend);
            currentLegend = null;
        }
    });
};

function getColor(d) {
    switch (d) {
        case "18- Nüfus":
            return "rgb(255, 0, 0)";
        case "18+ Nüfus":
            return "rgb(0, 255, 0)";
        case "AKP":
            return "rgb(255, 215, 0)";
        case "CHP":
            return "rgb(255, 0, 0)";
        case "MHP":
            return "rgb(0, 0, 255)";
        case "HDP":
            return "rgb(0, 100, 0)";
        case "Diğer":
            return "rgb(128, 128, 128)";
        default:
            return "rgb(0, 0, 0)";
    }
}