def hash(file_location, create_hash):
    with open(file_location, 'rb') as file:
        hash = create_hash()
        while chunk := file.read(4096):
            hash.update(chunk)

    return hash.hexdigest()
