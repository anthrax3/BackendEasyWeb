export function configure(aurelia) {
    aurelia.globalResources(
    	'./ValueConverter/take',
        './ValueConverter/currency-format',
        './ValueConverter/convert-to-image',
        './ValueConverter/to-length',
        './ValueConverter/json',
        './ValueConverter/take-from-to',
        './ValueConverter/date-format',
        './CustomAttributes/select2',
        './CustomAttributes/bootstrap-tooltip',
        './CustomAttributes/datetime-picker',
        './CustomAttributes/summernote',
        './ValueConverter/ObjectKeyConverter',
        './ValueConverter/number',
         './CustomAttributes/manific-popup',
        );
}
