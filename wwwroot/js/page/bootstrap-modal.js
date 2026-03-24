"use strict";

$("#modal-1").fireModal({body: 'Modal body text goes here.'});
$("#modal-2").fireModal({body: 'Modal body text goes here.', center: true});

let modal_3_body = '<p>Object to create a button on the modal.</p><pre class="language-javascript"><code>';
modal_3_body += '[\n';
modal_3_body += ' {\n';
modal_3_body += "   text: 'Login',\n";
modal_3_body += "   submit: true,\n";
modal_3_body += "   class: 'btn btn-primary btn-shadow',\n";
modal_3_body += "   handler: function(modal) {\n";
modal_3_body += "     alert('Hello, you clicked me!');\n"
modal_3_body += "   }\n"
modal_3_body += ' }\n';
modal_3_body += ']';
modal_3_body += '</code></pre>';
$("#modal-3").fireModal({
  title: 'Modal with Buttons',
  body: modal_3_body,
  buttons: [
    {
      text: 'Click, me!',
      class: 'btn btn-primary btn-shadow',
      handler: function(modal) {
        alert('Hello, you clicked me!');
      }
    }
  ]
});
$("#modal-4").fireModal({
  footerClass: 'bg-whitesmoke',
  body: 'Add the <code>bg-whitesmoke</code> class to the <code>footerClass</code> option.',
  buttons: [
    {
      text: 'No Action!',
      class: 'btn btn-primary btn-shadow',
      handler: function(modal) {
      }
    }
  ]
});
$("#modal-5").fireModal({
    title: 'Edit user',
    body: $("#modal-login-part"),
    footerClass: 'bg-whitesmoke',
    autoFocus: false,
    onFormSubmit: function (modal, e, form) {
        // Form Data
        let form_data = $(e.target).serialize();
        console.log(form_data)

        // DO AJAX HERE
        let fake_ajax = setTimeout(function () {
            form.stopProgress();
            modal.find('.modal-body').prepend('<div class="alert alert-info">Please check your browser console</div>')

            clearInterval(fake_ajax);
        }, 1500);

        e.preventDefault();
    },
    shown: function (modal, form) {
        console.log(form)
    },
 
});
$("#modal-6").fireModal({
    title: '',
    body: $("#modal-warehouse-part"),
    footerClass: 'bg-whitesmoke',
    autoFocus: false,
    onFormSubmit: function (modal, e, form) {
        // Form Data
        let form_data = $(e.target).serialize();
        console.log(form_data)

        // DO AJAX HERE
        let fake_ajax = setTimeout(function () {
            form.stopProgress();
            modal.find('.modal-body').prepend('<div class="alert alert-info">Please check your browser console</div>')

            clearInterval(fake_ajax);
        }, 1500);
            
        e.preventDefault();
    },
    shown: function (modal, form) {
        console.log(form)
    },
});
$("#modal-7").fireModal({
    title: '',
    body: $("#modal-detail-part"),
    footerClass: 'bg-whitesmoke',
    autoFocus: false,
    created: function (modal) {
        modal.find('.modal-dialog').css('max-width', '90%');
    },
    onFormSubmit: function (modal, e, form) {
        // Form Data
        let form_data = $(e.target).serialize();
        console.log(form_data)

        // DO AJAX HERE
        let fake_ajax = setTimeout(function () {
            form.stopProgress();
            modal.find('.modal-body').prepend('<div class="alert alert-info">Please check your browser console</div>')

            clearInterval(fake_ajax);
        }, 1500);

        e.preventDefault();
    },
    shown: function (modal, form) {
        console.log(form)
    },
});
$("#modal-8").fireModal({
    title: 'Lý do từ chối :',
    body: $("#modal-reject"),
    footerClass: 'bg-whitesmoke',
    autoFocus: false,
    created: function (modal) {
        modal.find('.modal-dialog').css('max-width', '380px');
        modal.find('.modal-dialog').css('padding-top', '300px');
        modal.find('.modal-content').css({
            'border': '1',
        });
    },
    onFormSubmit: function (modal, e, form) {
        // Form Data
        let form_data = $(e.target).serialize();
        console.log(form_data)

        // DO AJAX HERE
        let fake_ajax = setTimeout(function () {
            form.stopProgress();
            modal.find('.modal-body').prepend('<div class="alert alert-info">Please check your browser console</div>')

            clearInterval(fake_ajax);
        }, 1500);

        e.preventDefault();
    },
    shown: function (modal, form) {
        console.log(form)
    },
});
$("#modal-9").fireModal({
    title: '',
    body: $("#modal-download"),
    footerClass: 'bg-whitesmoke',
    autoFocus: false,
    created: function (modal) {
        modal.find('.modal-dialog').css('max-width', '500px');
        modal.find('.modal-dialog').css('padding-top', '300px');
        modal.find('.modal-content').css({
            'border': '1',
        });
    },
    onFormSubmit: function (modal, e, form) {
        // Form Data
        let form_data = $(e.target).serialize();
        console.log(form_data)

        // DO AJAX HERE
        let fake_ajax = setTimeout(function () {
            form.stopProgress();
            modal.find('.modal-body').prepend('<div class="alert alert-info">Please check your browser console</div>')

            clearInterval(fake_ajax);
        }, 1500);

        e.preventDefault();
    },
    shown: function (modal, form) {
        console.log(form)
    },
});
$("#modal-10").fireModal({
    title: ' ',
    body: $("#modal-xuatkho"),
    footerClass: 'bg-whitesmoke',
    autoFocus: false,
    created: function (modal) {
        modal.find('.modal-dialog').css('max-width', '800px');
        modal.find('.modal-dialog').css('padding-top', '200px');
        modal.find('.modal-content').css({
            'border': '1',
        });
    },
    onFormSubmit: function (modal, e, form) {
        // Form Data
        let form_data = $(e.target).serialize();
        console.log(form_data)

        // DO AJAX HERE
        let fake_ajax = setTimeout(function () {
            form.stopProgress();
            modal.find('.modal-body').prepend('<div class="alert alert-info">Please check your browser console</div>')

            clearInterval(fake_ajax);
        }, 1500);

        e.preventDefault();
    },
    shown: function (modal, form) {
        console.log(form)
    },
});
$("#modal-11").fireModal({
    title: '',
    body: $("#modal-chuyenkho"),
    footerClass: 'bg-whitesmoke',
    autoFocus: false,
    created: function (modal) {
        modal.find('.modal-dialog').css('max-width', '600px');
        modal.find('.modal-dialog').css('padding-top', '200px');
        modal.find('.modal-content').css({
            'border': '1',
        });
    },
    onFormSubmit: function (modal, e, form) {
        // Form Data
        let form_data = $(e.target).serialize();
        console.log(form_data)

        // DO AJAX HERE
        let fake_ajax = setTimeout(function () {
            form.stopProgress();
            modal.find('.modal-body').prepend('<div class="alert alert-info">Please check your browser console</div>')

            clearInterval(fake_ajax);
        }, 1500);

        e.preventDefault();
    },
    shown: function (modal, form) {
        console.log(form)
    },
});
$("#modal-12").fireModal({
    title: '',
    body: $("#modal-wh-add"),
    footerClass: 'bg-whitesmoke',
    autoFocus: false,
    created: function (modal) {
        modal.find('.modal-dialog').css('max-width', '600px');
        modal.find('.modal-dialog').css('padding-top', '200px');
        modal.find('.modal-content').css({
            'border': '1',
        });
    },
    onFormSubmit: function (modal, e, form) {
        // Form Data
        let form_data = $(e.target).serialize();
        console.log(form_data)

        // DO AJAX HERE
        let fake_ajax = setTimeout(function () {
            form.stopProgress();
         
            clearInterval(fake_ajax);
        }, 1500);

        e.preventDefault();
    },
    shown: function (modal, form) {
        console.log(form)
    },
});
$("#modal-13").fireModal({
    title: '',
    body: $("#modal-tainhap"),
    footerClass: 'bg-whitesmoke',
    autoFocus: false,
    created: function (modal) {
        modal.find('.modal-dialog').css('max-width', '600px');
        modal.find('.modal-dialog').css('padding-top', '200px');
        modal.find('.modal-content').css({
            'border': '1',
        });
    },
    onFormSubmit: function (modal, e, form) {
        // Form Data
        let form_data = $(e.target).serialize();
        console.log(form_data)

        // DO AJAX HERE
        let fake_ajax = setTimeout(function () {
            form.stopProgress();

            clearInterval(fake_ajax);
        }, 1500);

        e.preventDefault();
    },
    shown: function (modal, form) {
        console.log(form)
    },
});
$("#modal-15").fireModal({
    title: '',
    body: $("#modal-silder"),
    footerClass: 'bg-whitesmoke',
    autoFocus: false,
    created: function (modal) {
        modal.find('.modal-dialog').css('max-width', '90%');
        modal.find('.modal-content').css({
            'border': '1',
        });
    },
    onFormSubmit: function (modal, e, form) {
        // Form Data
        let form_data = $(e.target).serialize();
        console.log(form_data)

        // DO AJAX HERE
        let fake_ajax = setTimeout(function () {
            form.stopProgress();

            clearInterval(fake_ajax);
        }, 1500);

        e.preventDefault();
    },
    shown: function (modal, form) {
        console.log(form)
    },
});
$("#modal-16").fireModal({
    title: '',
    body: $("#modal-bell"),
    footerClass: 'bg-whitesmoke',
    autoFocus: false,
    created: function (modal) {
        modal.find('.modal-dialog').css('max-width', '56%');
        modal.find('.modal-content').css({
            'border': '1',
        });
    },
    onFormSubmit: function (modal, e, form) {
     
        // DO AJAX HERE
        let fake_ajax = setTimeout(function () {
            form.stopProgress();

            clearInterval(fake_ajax);
        }, 1500);

        e.preventDefault();
    },
    shown: function (modal, form) {
        console.log(form)
    },
});
$("#modal-17").fireModal({
    title: '',
    body: $("#modal-truyxuat"),
    footerClass: 'bg-whitesmoke',
    autoFocus: false,
    created: function (modal) {
        modal.find('.modal-dialog').css('max-width', '86%');
        modal.find('.modal-content').css({
            'border': '1',
        });
    },
    onFormSubmit: function (modal, e, form) {

        // DO AJAX HERE
        let fake_ajax = setTimeout(function () {
            form.stopProgress();

            clearInterval(fake_ajax);
        }, 1500);

        e.preventDefault();
    },
    shown: function (modal, form) {
        console.log(form)
    },
});
$("#modal-18").fireModal({
    title: '',
    body: $("#modal-giaohang"),
    footerClass: 'bg-whitesmoke',
    autoFocus: false,
    created: function (modal) {
        modal.find('.modal-dialog').css('max-width', '600px');
        modal.find('.modal-dialog').css('padding-top', '200px');
        modal.find('.modal-content').css({
            'border': '1',
        });
    },
    onFormSubmit: function (modal, e, form) {
        // Form Data
        let form_data = $(e.target).serialize();
        console.log(form_data)

        // DO AJAX HERE
        let fake_ajax = setTimeout(function () {
            form.stopProgress();

            clearInterval(fake_ajax);
        }, 1500);

        e.preventDefault();
    },
    shown: function (modal, form) {
        console.log(form)
    },
});
$("#modal-19").fireModal({
    title: '',
    body: $("#modal-hienchitiet"),
    footerClass: 'bg-whitesmoke',
    autoFocus: false,
    created: function (modal) {
        modal.find('.modal-dialog').css('max-width', '86%');
        modal.find('.modal-content').css({
            'border': '1',
        });
    },
    onFormSubmit: function (modal, e, form) {

        // DO AJAX HERE
        let fake_ajax = setTimeout(function () {
            form.stopProgress();

            clearInterval(fake_ajax);
        }, 1500);

        e.preventDefault();
    },
    shown: function (modal, form) {
        console.log(form)
    },
});
$("#modal-20").fireModal({
    title: '',
    body: $("#modal-warehouse-part-GA"),
    footerClass: 'bg-whitesmoke',
    autoFocus: false,
    onFormSubmit: function (modal, e, form) {
        // Form Data
        let form_data = $(e.target).serialize();
        console.log(form_data)

        // DO AJAX HERE
        let fake_ajax = setTimeout(function () {
            form.stopProgress();
            modal.find('.modal-body').prepend('<div class="alert alert-info">Please check your browser console</div>')

            clearInterval(fake_ajax);
        }, 1500);

        e.preventDefault();
    },
    shown: function (modal, form) {
        console.log(form)
    },
});
$('.oh-my-modal').fireModal({
  title: 'My Modal',
  body: 'This is cool plugin!'
});

$("#modal-vd-add").fireModal({
    title: 'Thêm nhà cung cấp',
    body: $("#modal-vender-add"),
    footerClass: 'bg-whitesmoke',
    autoFocus: false,
    onFormSubmit: function (modal, e, form) {
        // Form Data
        let form_data = $(e.target).serialize();
        
        // DO AJAX HERE
        let fake_ajax = setTimeout(function () {
            form.stopProgress();
            modal.find('.modal-body').prepend('<div class="alert alert-info">Please check your browser console</div>')

            clearInterval(fake_ajax);
        }, 1500);

        e.preventDefault();
    },
    shown: function (modal, form) {
        console.log(form)
    },
    buttons: [
        {
            text: 'Save',
            onclick: '_update_vender()',
            submit: true,
            class: 'btn btn-primary btn-shadow',
            handler: function (modal) {
            }
        }
    ]
});
