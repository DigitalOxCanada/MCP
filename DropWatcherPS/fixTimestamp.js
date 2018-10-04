//find all DTs that are strings and convert them to Dates
db.packages.find({ DT: { $type: 2 } }).forEach(function (doc) {
    doc.DT = new ISODate(doc.DT);
    db.packages.save(doc);
})
