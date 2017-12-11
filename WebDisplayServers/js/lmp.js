function requestData(selector) {
  $.ajax({
      type: 'GET',
      url: 'http://37.59.50.110:8701',
      dataType: 'jsonp',
      success: function(json) {
      	buildHtmlTable(json, selector);
      },
	  error: function(json) {
      	$(selector).append("ERROR");
      }
  });
}

// Builds the HTML Table out of myList.
function buildHtmlTable(json, selector) {
	var columns = addAllColumnHeaders(json, selector);

	for (var i = 0; i < json.length; i++) {
		var row$ = $('<tr/>');
		for (var colIndex = 0; colIndex < columns.length; colIndex++) {
			var cellValue = json[i][columns[colIndex]];
			if (cellValue == null) cellValue = "";
			row$.append($('<td/>').html(cellValue));
		}
		$(selector).append(row$);
	}
}

// Adds a header row to the table and returns the set of columns.
// Need to do union of keys from all records as some records may not contain
// all records.
function addAllColumnHeaders(json, selector) {
	var columnSet = [];
	var headerTr$ = $('<tr/>');

	for (var i = 0; i < json.length; i++) {
		var rowHash = json[i];
		for (var key in rowHash) {
			if ($.inArray(key, columnSet) == -1) {
				columnSet.push(key);
				headerTr$.append($('<th/>').html(key));
			}
		}
	}
	$(selector).append(headerTr$);
	return columnSet;
}