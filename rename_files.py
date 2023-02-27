# importing os module
import os
 
# Function to rename multiple files
def main():
   
    folder = "Slider/Assets/Animations/Characters/NPC/Village"
    for count, filename in enumerate(os.listdir(folder)):
        dst = f"Hostel {str(count)}.jpg"
        src =f"{folder}/{filename}"  # foldername/filename, if .py file is outside folder
        dst =f"{folder}/{dst}"

        if '_v2' in src:
            d1, d2 = src.split("_v2")
            print(d1 + d2)
            dst = d1 + d2
             
            # rename() function will
            # rename all the files
            os.rename(src, dst)
 
# Driver Code
if __name__ == '__main__':
     
    # Calling main() function
    main()