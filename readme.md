# Document OCR
My HOA provided the HOA documents as a scanned PDF. 160+ pages I am not able to Ctrl+F on. Passed the full document through Azure OCR to extract plain text I can Ctrl+F on. I built this program to loop through each page (an image) of the document, call to the Azure service that performs OCR, and then save.

Original documents:
- [Full HOA Documents PDF](https://github.com/TimHanewich/doc-ocr/releases/download/1/Palmero-HOA.pdf)
- [The full PDF split into individual JPEG images](https://github.com/TimHanewich/doc-ocr/releases/download/1/images.zip)

Resulting documents:
- All `ImageReadTask` objects ([this class](./src/ImageReadTask.cs)), containing the page number and read OCR result pairs: [result.json](./results/result.json).
- [The full document](./results/result.txt).