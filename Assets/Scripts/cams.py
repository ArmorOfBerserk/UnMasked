import cv2

def list_ports():
    is_working = True
    dev_port = 0
    working_ports = []
    while is_working:
        camera = cv2.VideoCapture(dev_port)
        if not camera.isOpened():
            is_working = False
            print(f"Porta {dev_port} non attiva/non trovata.")
        else:
            is_reading, img = camera.read()
            if is_reading:
                print(f"Porta {dev_port} è funzionante (Camera trovata).")
                working_ports.append(dev_port)
            else:
                print(f"Porta {dev_port} aperta ma non legge frame.")
        dev_port += 1
    return working_ports

print("Scansione porte video...")
list_ports()