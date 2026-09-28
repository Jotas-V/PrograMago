from pathlib import Path

from docx import Document


document_path = Path(
    r"C:\Users\Pichau\Documents\TCC\PrograMago\docs\PrograMago_artigo_introducao_metodos.docx"
)
title = (
    "Desenvolvimento e avaliação de um protótipo de jogo educativo para a "
    "aprendizagem de Programação Orientada a Objetos"
)

document = Document(document_path)
paragraph = document.paragraphs[0]
assert paragraph.style.name == "Title"
assert len(paragraph.runs) == 1
paragraph.runs[0].text = title
document.save(document_path)
print(paragraph.text)
