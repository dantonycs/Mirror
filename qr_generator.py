import os
import qrcode

# Path to the folder where images are hosted in the repository
image_folder = './last_screenshot'

# Path to the folder where QR codes will be saved
qr_code_folder = './qr_image'

# Iterate through each image in the image folder
for image_filename in os.listdir(image_folder):
    if image_filename.lower().endswith('.jpg') or image_filename.lower().endswith('.png'):
        # Create a QR code instance
        qr = qrcode.QRCode(
            version=1,
            error_correction=qrcode.constants.ERROR_CORRECT_L,
            box_size=10,
            border=4,
        )
        # URL to the raw image file on GitHub
        image_url = f'https://github.com/dantonycs/MirrorQR/blob/main/{image_folder}/{image_filename}?raw=true'
        qr.add_data(image_url)
        qr.make(fit=True)

        # Create an image from the QR code instance
        qr_image = qr.make_image(fill_color="black", back_color="white")

        # Save the QR code image
        qr_image.save(os.path.join(qr_code_folder, f'{image_filename}'))
