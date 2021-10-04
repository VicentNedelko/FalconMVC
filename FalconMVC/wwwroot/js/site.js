// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

'use strict';

var intTable = document.querySelector('#interfaceList');
var radios = intTable.querySelectorAll('td > input');
radios.forEach(radios =>
    radios.addEventListener('change', function () { FillInterfaceForm(this.value) })
);

function FillInterfaceForm(ip) {
    const selectedInterface = document.querySelector('#selectedInterfaceIP');
    selectedInterface.innerHTML = ip;
    var intData = document.getElementById('foundInterfaces');
    intData.style.visibility = 'visible';
    document.querySelector('#intLabel').style.visibility = 'visible';
    document.querySelector('#selectedIP').value = ip;
    document.getElementById('submitBtn').style.visibility = 'visible';
}
