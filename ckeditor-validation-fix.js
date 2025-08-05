// Fixed CKEditor initialization with proper jQuery Validation integration
ClassicEditor.create(document.querySelector('#ProjectSummary'), {
    removePlugins: ['CKFinderUploadAdapter', 'CKFinder', 'EasyImage', 'Image', 'ImageCaption', 'ImageStyle', 'ImageToolbar', 'ImageUpload', 'MediaEmbed'],
    toolbar: [
        'heading', '|',
        'bold', 'italic', 'link', '|',
        'numberedList', 'bulletedList', '|',
        'insertTable', '|',
        'undo', 'redo'
    ],
    language: 'ar',
    contentLanguage: 'ar'
})
.then(editor => {
    window.editor = editor;
    
    // Get the original textarea element
    const originalElement = document.querySelector('#ProjectSummary');
    
    // Enable manual resizing with CSS - vertical only
    const editorElement = editor.ui.getEditableElement();
    editorElement.style.resize = 'vertical';
    editorElement.style.overflow = 'auto';
    editorElement.style.height = '75px';
    editorElement.style.minHeight = '75px';
    
    // Exclude CKEditor elements from jQuery validation
    if (window.jQuery && jQuery.validator) {
        // Add custom ignore rule for CKEditor elements
        jQuery.validator.setDefaults({
            ignore: ".ck-editor__editable, .ck *"
        });
        
        // Ensure the original element has a proper name attribute for validation
        if (!originalElement.name && originalElement.id) {
            originalElement.name = originalElement.id;
        }
    }
    
    // Fix for jQuery Validate - update hidden field on content change
    editor.model.document.on('change:data', () => {
        const data = editor.getData();
        originalElement.value = data;
        
        // Trigger validation on the original element only if validator exists
        if (window.jQuery && jQuery('#ProjectSummary').length > 0) {
            const form = jQuery('#ProjectSummary').closest('form');
            const validator = form.data('validator');
            if (validator) {
                // Clear any existing errors first
                validator.resetForm();
                // Validate the specific element
                validator.element('#ProjectSummary');
            }
        }
    });
    
    // Also update on blur to ensure validation works
    editor.ui.focusTracker.on('change:isFocused', (evt, data, isFocused) => {
        if (!isFocused) {
            const data = editor.getData();
            originalElement.value = data;
            
            if (window.jQuery && jQuery('#ProjectSummary').length > 0) {
                const form = jQuery('#ProjectSummary').closest('form');
                const validator = form.data('validator');
                if (validator) {
                    // Use setTimeout to ensure the blur event completes first
                    setTimeout(() => {
                        validator.element('#ProjectSummary');
                    }, 100);
                }
            }
        }
    });
    
    // Initial sync of content
    const initialData = editor.getData();
    originalElement.value = initialData;
})
.catch(err => {
    console.error('CKEditor initialization error:', err.stack);
});

// Additional jQuery Validation configuration to handle CKEditor properly
if (window.jQuery && jQuery.validator) {
    // Override the default focusout handler to prevent validation errors on CKEditor elements
    jQuery.validator.defaults.onfocusout = function(element) {
        // Skip validation for CKEditor elements
        if (jQuery(element).hasClass('ck-editor__editable') || 
            jQuery(element).closest('.ck-editor').length > 0) {
            return;
        }
        
        // Call the original validation logic for other elements
        if (!this.checkable(element) && (element.name in this.submitted || !this.optional(element))) {
            this.element(element);
        }
    };
}